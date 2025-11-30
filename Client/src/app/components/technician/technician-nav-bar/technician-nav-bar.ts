import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateSelect } from '../../shared/translate-select/translate-select';
import { TechnicianService } from '../../../core/services/technician-service';
import { AuthService } from '../../../core/services/auth-service';
import { NotificationService } from '../../../core/services/notification-service';

@Component({
  selector: 'app-technician-nav-bar',
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateSelect, TranslateModule],
  templateUrl: './technician-nav-bar.html',
  styleUrl: './technician-nav-bar.css',
})
export class TechnicianNavBar implements OnInit {
  mobileMenuOpen = false;
  userMenuOpen = false;
  notifications: any[] = [];
  unread = 0;
  showNotifications = false;
  user = {
    name: 'Technician',
    imageUrl: null as string | null,
  };

  constructor(
    private router: Router,
    private elementRef: ElementRef<HTMLElement>,
    private technicianService: TechnicianService,
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadUserProfile();
    const token = this.authService.getToken();
        // Start SignalR
    if(token)
    this.notificationService.startConnection(token);

    // Load initial notifications
    this.notificationService.loadUserNotifications();

    // Subscribe to updates
    this.notificationService.notifications$.subscribe(list => {
      //console.log('Notifications updated:', list);
      this.notifications = list;
    });

    this.notificationService.unreadCount$.subscribe(count => {
      this.unread = count;
    });
  }
  toggleNotifications() {
  this.showNotifications = !this.showNotifications;

  if (this.showNotifications) {
    // 🔥 smart fetch (cached if no updates)
    this.notificationService.loadUserNotifications();

    // If unread exists, mark as read
    if (this.unread > 0) {
      this.notificationService.markAllRead().subscribe(() => {
        this.notificationService.markAllAsRead(); // UI instantly updated
      });
    }
  }
}


openNotification(n: any) {
  this.router.navigateByUrl(n.actionUrl);
}

  get userInitials(): string {
    return this.computeInitials(this.user.name);
  }

  toggleMenu(): void {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  toggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.userMenuOpen = !this.userMenuOpen;
  }

  closeUserMenu(): void {
    this.userMenuOpen = false;
  }

  navigate(path: string): void {
    this.router.navigate([path]);
    this.closeMobileMenu();
    this.closeUserMenu();
  }

  logout(): void {
    this.closeMobileMenu();
    this.closeUserMenu();
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.elementRef.nativeElement.contains(event.target as Node)) {
      this.closeUserMenu();
    }
  }

  private computeInitials(fullName: string): string {
    if (!fullName) {
      return '--';
    }

    return fullName
      .split(' ')
      .filter(Boolean)
      .map((segment) => segment.charAt(0).toUpperCase())
      .slice(0, 2)
      .join('');
  }

  private loadUserProfile(): void {
    const claims = this.technicianService.getTechnicianTokenClaims();
    const fallbackName = claims.fullName?.trim() || 'Technician';
    const technicianId = Number(claims.nameIdentifier);
    this.user.name = fallbackName;
    if (!technicianId) {
      this.user.name = fallbackName;
      return;
    }

    this.technicianService.getTechnicianById(technicianId).subscribe({
      next: (response) => {
        const fetchedName = response?.data?.fullName?.trim();
        this.user.name = fetchedName || fallbackName;
        this.user.imageUrl = response?.data?.profileImageUrl ?? null;
      },
      error: () => {
        this.user.name = fallbackName;
      },
    });
  }
}
