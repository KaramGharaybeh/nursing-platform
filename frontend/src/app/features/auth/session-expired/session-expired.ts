import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';

@Component({
  selector: 'np-session-expired',
  imports: [RouterLink],
  templateUrl: './session-expired.html',
  styleUrl: './session-expired.scss',
})
export class SessionExpired {
  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
}
