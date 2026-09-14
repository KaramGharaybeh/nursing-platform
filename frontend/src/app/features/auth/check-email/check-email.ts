import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';

@Component({
  selector: 'np-check-email',
  imports: [RouterLink],
  templateUrl: './check-email.html',
  styleUrl: './check-email.scss',
})
export class CheckEmail {
  protected readonly signInPath = canonicalRoutePath('AUTH_SIGN_IN');
}
