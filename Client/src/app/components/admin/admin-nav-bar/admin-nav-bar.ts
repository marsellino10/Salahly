import { Component, ElementRef, HostListener } from '@angular/core';
import { NavigationExtras, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth-service';
import { CustomerService } from '../../../core/services/customer-service';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateSelect } from '../../shared/translate-select/translate-select';
import { CommonModule } from '@angular/common';
import { AdminService } from '../../../core/services/admin-service';

@Component({
  selector: 'app-admin-nav-bar',
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule, TranslateSelect],
  templateUrl: './admin-nav-bar.html',
  styleUrl: './admin-nav-bar.css',
})
export class AdminNavBar {
  mobileMenuOpen = false;
  userMenuOpen = false;

  user = {
    name: 'Admin',
    imageUrl: null as string | null,
  };

  constructor(
    private router: Router,
    private elementRef: ElementRef<HTMLElement>,
    private adminService: AdminService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.loadUserProfile();
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

  navigate(path: string, extras?: NavigationExtras): void {
    this.router.navigate([path], extras);
    this.closeMobileMenu();
    this.closeUserMenu();
  }

  navigateToRequests(): void {
    this.router.navigate(['/approve-portfolio-item']);
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
    const claims = this.adminService.getAdminTokenClaims();
    const fallbackName = claims.fullName?.trim() || 'Admin';
    const customerId = Number(claims.nameIdentifier);
    this.user.name = fallbackName;
  }
}
