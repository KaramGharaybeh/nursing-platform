import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';

@Component({
  selector: 'np-role-selection',
  imports: [MatButtonModule, RouterLink],
  templateUrl: './role-selection.html',
  styleUrl: './role-selection.scss',
})
export class RoleSelection {
  private readonly router = inject(Router);

  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');

  protected registerAsNurse(): void {
    void this.router.navigateByUrl(canonicalRoutePath('AUTH_REGISTER_NURSE'));
  }

  protected registerAsEmployer(): void {
    void this.router.navigateByUrl(canonicalRoutePath('AUTH_REGISTER_EMPLOYER'));
  }
}
