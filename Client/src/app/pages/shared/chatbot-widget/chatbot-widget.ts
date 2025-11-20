import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  ViewChild,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ChatReply,
  FireworksChatService,
} from '../../../core/services/fireworks-chat.service';

type ChatRole = 'user' | 'assistant';

interface ChatMessage {
  role: ChatRole;
  content: string;
  timestamp: Date;
  pending?: boolean;
}

@Component({
  selector: 'app-chatbot-widget',
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot-widget.html',
  styleUrl: './chatbot-widget.css',
})
export class ChatbotWidget {
  private readonly _chatService = inject(FireworksChatService);

  @ViewChild('messagesContainer')
  private messagesContainer?: ElementRef<HTMLDivElement>;

  isPanelOpen = signal(false);
  isMinimized = signal(false);
  isSending = signal(false);
  errorMessage = signal<string | null>(null);
  messageDraft = signal('');
  messages = signal<ChatMessage[]>([
    {
      role: 'assistant',
      content: 'Hi! I\'m Salahly\'s virtual assistant. How can I help you today?',
      timestamp: new Date(),
    },
  ]);

  readonly hasMessages = computed(() => this.messages().length > 0);

  togglePanel(): void {
    const shouldOpen = !this.isPanelOpen();
    this.isPanelOpen.set(shouldOpen);
    if (shouldOpen) {
      this.isMinimized.set(false);
      queueMicrotask(() => this.scrollToBottom());
    }
  }

  closePanel(): void {
    this.isPanelOpen.set(false);
    this.isMinimized.set(false);
  }

  toggleMinimize(): void {
    if (!this.isPanelOpen()) {
      return;
    }
    this.isMinimized.update((value) => !value);
  }

  sendMessage(): void {
    const draft = this.messageDraft().trim();
    if (!draft || this.isSending()) {
      return;
    }

    const userMessage: ChatMessage = {
      role: 'user',
      content: draft,
      timestamp: new Date(),
    };

    this.messages.update((current) => [...current, userMessage]);
    this.messageDraft.set('');
    this.errorMessage.set(null);
    queueMicrotask(() => this.scrollToBottom());

    const placeholder: ChatMessage = {
      role: 'assistant',
      content: '...',
      pending: true,
      timestamp: new Date(),
    };

    this.messages.update((current) => [...current, placeholder]);
    this.isSending.set(true);

    const conversationContext = this.buildContext();

    this._chatService.sendMessage(draft, conversationContext).subscribe({
      next: (reply: ChatReply) => {
        this.isSending.set(false);
        if (reply.isFallback) {
          this.errorMessage.set(
            "I'm not sure, this information is not available in the current version of the app.",
          );
        }

        const answer = reply.answer || "I'm sorry, I could not generate a response right now. Please try again.";
        this.replacePendingMessage(answer);
      },
      error: (error: Error) => {
        this.isSending.set(false);
        const message =
          error.message ||
          'Something went wrong while contacting our assistant. Please try again later.';
        this.errorMessage.set(message);
        this.replacePendingMessage(
          'I\'m sorry, I ran into an issue. Please try again in a moment or contact our support team.',
        );
      },
    });
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  trackByIndex(_: number, item: ChatMessage): number {
    return item.timestamp.getTime();
  }

  private buildContext(): string {
    const history = this.messages()
      .filter((message) => !message.pending)
      .slice(-6)
      .map((message) => `${message.role.toUpperCase()}: ${message.content}`);

    return history.join('\n');
  }

  private replacePendingMessage(content: string): void {
    this.messages.update((current) => {
      const updated = [...current];
      for (let i = updated.length - 1; i >= 0; i--) {
        const item = updated[i];
        if (item.pending) {
          updated[i] = {
            role: 'assistant',
            content,
            timestamp: new Date(),
          };
          break;
        }
      }
      return updated;
    });

    queueMicrotask(() => this.scrollToBottom());
  }

  private scrollToBottom(): void {
    if (!this.messagesContainer) {
      return;
    }

    requestAnimationFrame(() => {
      const element = this.messagesContainer?.nativeElement;
      if (element) {
        element.scrollTop = element.scrollHeight;
      }
    });
  }
}
