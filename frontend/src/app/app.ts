import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LocaleDirectionService } from './core/locale/locale-direction.service';

@Component({
  selector: 'np-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly localeDirection = inject(LocaleDirectionService);
  protected readonly title = signal('nursing-platform-frontend');
}
