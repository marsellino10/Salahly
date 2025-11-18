import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, OnInit } from '@angular/core';
import { NavigationExtras, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateSelect } from '../../shared/translate-select/translate-select';
import { CustomerService } from '../../../core/services/customer-service';
import { AuthService } from '../../../core/services/auth-service';

@Component({
  selector: 'app-customer-nav-bar',
  imports: [CommonModule, RouterLink, RouterLinkActive, TranslateModule, TranslateSelect],
  templateUrl: './customer-nav-bar.html',
  styleUrl: './customer-nav-bar.css',
})
export class CustomerNavBar implements OnInit {
  mobileMenuOpen = false;
  userMenuOpen = false;

  user = {
    name: 'Customer',
    imageUrl: null as string | null,
  };

  constructor(
    private router: Router,
    private elementRef: ElementRef<HTMLElement>,
    private customerService: CustomerService,
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
    this.router.navigate(['/show-services-requested']);
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
    const claims = this.customerService.getCustomerTokenClaims();
    const fallbackName = claims.fullName?.trim() || 'Customer';
    const customerId = Number(claims.nameIdentifier);
    this.user.name = fallbackName;
    if (!customerId) {
      return;
    }

    this.customerService.getCustomerById(customerId).subscribe({
      next: (response) => {
        const fetchedName = response?.fullName?.trim();
        this.user.name = fetchedName || fallbackName;
        this.user.imageUrl = response?.profileImageUrl ?? null;
      },
      error: () => {
        this.user.name = fallbackName;
      },
    });
  }
}
