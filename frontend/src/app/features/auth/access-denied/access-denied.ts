import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { canonicalRoutePath } from '../../../core/routing/canonical-routes';

@Component({
  selector: 'np-access-denied',
  imports: [RouterLink],
  templateUrl: './access-denied.html',
  styleUrl: './access-denied.scss',
})
export class AccessDenied {
  protected readonly accountPath = canonicalRoutePath('ACCOUNT_OVERVIEW');
}
