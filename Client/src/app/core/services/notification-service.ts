import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private hubConnection!: signalR.HubConnection;

  private notificationsSubject = new BehaviorSubject<any[]>([]);
  notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  private baseUrl = "http://localhost:5049/api/notification";

  constructor(private http: HttpClient) {}

  /** Start SignalR connection */
  startConnection(token: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5049/notificationHub", {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(err => console.log("SignalR Error", err));

    // In startConnection method, update the SignalR listener:
this.hubConnection.on("ReceiveNotification", () => {
  // Just increment the unread count - no need to add to notifications array
  this.unreadCountSubject.next(
    this.unreadCountSubject.value + 1
  );
});
  }

  /** Load from backend */
/** Load notifications only if needed */
loadUserNotifications() {
  const current = this.notificationsSubject.value;
  const unread = this.unreadCountSubject.value;

  // 🚀 If we already have notifications AND unread=0, nothing changed → DO NOT call API
  if (current.length > 0 && unread === 0) {
    return; // use cached data
  }

  // Otherwise, call API
  this.http.get<any[]>(`${this.baseUrl}/user`).subscribe(data => {
    this.notificationsSubject.next(data);
    console.log('Notifications loaded:', data);
    const unread = data.filter(n => !n.isRead).length;
    this.unreadCountSubject.next(unread);
  });
}


  markRead(id: number) {
  return this.http.post(`${this.baseUrl}/mark-read/${id}`, {});
}

markAllRead() {
  return this.http.post(`${this.baseUrl}/mark-all-read`, {});
}

  markAllAsRead() {
    const updated = this.notificationsSubject.value.map(n => ({
      ...n,
      isRead: true
    }));

    this.notificationsSubject.next(updated);
    this.unreadCountSubject.next(0);
  }
}


