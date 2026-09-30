import { Component, computed, inject, input, output, signal } from '@angular/core';
import { LocalizationService } from '../../../core/i18n/localization.service';

@Component({
  selector: 'np-auth-text-field',
  templateUrl: './auth-text-field.html',
  styleUrl: './auth-text-field.scss',
})
export class AuthTextField {
  readonly controlId = input.required<string>();
  readonly label = input.required<string>();
  readonly type = input<'text' | 'email' | 'password'>('text');
  readonly value = input('');
  readonly placeholder = input('');
  readonly helperText = input('');
  readonly errorText = input('');
  readonly required = input(false);
  readonly disabled = input(false);
  readonly showVisibilityToggle = input(false);
  readonly valueChange = output<string>();
  protected readonly i18n = inject(LocalizationService);
  protected readonly passwordVisible = signal(false);
  protected readonly inputType = computed(() => this.type() === 'password' && this.passwordVisible() ? 'text' : this.type());

  protected toggleVisibility(): void {
    this.passwordVisible.update((visible) => !visible);
  }

  protected updateValue(event: Event): void {
    this.valueChange.emit((event.target as HTMLInputElement).value);
  }
}
