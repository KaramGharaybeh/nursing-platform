import { Component, input, output } from '@angular/core';

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
  readonly valueChange = output<string>();

  protected updateValue(event: Event): void {
    this.valueChange.emit((event.target as HTMLInputElement).value);
  }
}
