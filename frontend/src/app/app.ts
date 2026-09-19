import { Component, DestroyRef, inject, signal } from '@angular/core';
import {
  NavigationCancel,
  NavigationEnd,
  NavigationError,
  NavigationStart,
  Router,
  RouterOutlet,
} from '@angular/router';
import { LocaleDirectionService } from './core/locale/locale-direction.service';
import { LoadingErrorRetry } from './shared/ui/loading-error-retry';

@Component({
  selector: 'np-root',
  imports: [RouterOutlet, LoadingErrorRetry],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly localeDirection = inject(LocaleDirectionService);
  private readonly router = inject(Router);

  protected readonly navigating = signal(false);

  constructor() {
    const subscription = this.router.events.subscribe((event) => {
      if (event instanceof NavigationStart) {
        this.navigating.set(true);
      } else if (
        event instanceof NavigationEnd ||
        event instanceof NavigationCancel ||
        event instanceof NavigationError
      ) {
        this.navigating.set(false);
      }
    });
    inject(DestroyRef).onDestroy(() => subscription.unsubscribe());
  }
}
