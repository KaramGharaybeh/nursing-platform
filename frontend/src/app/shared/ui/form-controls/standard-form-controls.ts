import { Component, Directive, EventEmitter, Input, Output } from '@angular/core';
import { MatCheckboxChange, MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatRadioChange, MatRadioModule } from '@angular/material/radio';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';

export type NpTextInputType = 'text' | 'email' | 'password' | 'search' | 'number';
type NpStandardControlKind = 'input' | 'textarea' | 'select' | 'checkbox' | 'radio' | 'date';

export interface NpSelectOption {
  readonly value: string;
  readonly label: string;
  readonly disabled?: boolean;
}

let nextControlId = 0;

const STANDARD_FORM_CONTROL_IMPORTS = [MatCheckboxModule, MatFormFieldModule, MatInputModule, MatRadioModule, MatSelectModule];

@Directive()
abstract class NpStandardControlBase {
  protected abstract readonly controlKind: NpStandardControlKind;

  @Input() controlId = `np-standard-control-${++nextControlId}`;
  @Input() label = '';
  @Input() helperText = '';
  @Input() errorText = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() optionalLabel = 'Optional';
  @Input() placeholder = '';
  @Input() value = '';
  @Input() readonly = false;
  @Input() rows = 3;
  @Input() type: NpTextInputType = 'text';
  @Input() options: readonly NpSelectOption[] = [];
  @Input() checked = false;

  @Output() readonly valueChange = new EventEmitter<string>();
  @Output() readonly checkedChange = new EventEmitter<boolean>();

  protected get helperId(): string {
    return `${this.controlId}-helper`;
  }

  protected get errorId(): string {
    return `${this.controlId}-error`;
  }

  protected get hasHelper(): boolean {
    return this.helperText.trim() !== '';
  }

  protected get hasError(): boolean {
    return this.errorText.trim() !== '';
  }

  protected get requirementLabel(): string | undefined {
    return this.required ? undefined : this.optionalLabel;
  }

  protected get describedBy(): string | undefined {
    const ids = [this.hasHelper ? this.helperId : undefined, this.hasError ? this.errorId : undefined].filter(
      (id): id is string => id !== undefined,
    );

    return ids.length > 0 ? ids.join(' ') : undefined;
  }

  protected updateTextValue(value: string): void {
    this.valueChange.emit(value);
  }

  protected updateSelectValue(event: MatSelectChange): void {
    this.valueChange.emit(String(event.value));
  }

  protected updateChecked(event: MatCheckboxChange): void {
    this.checkedChange.emit(event.checked);
  }

  protected updateRadioValue(event: MatRadioChange): void {
    this.valueChange.emit(String(event.value));
  }

  protected selectedOptionLabel(): string {
    return this.options.find((option) => option.value === this.value)?.label ?? '';
  }
}

@Component({
  selector: 'np-text-input-control',
  imports: STANDARD_FORM_CONTROL_IMPORTS,
  templateUrl: './standard-form-controls.html',
  styleUrl: './standard-form-controls.scss',
})
export class NpTextInputControl extends NpStandardControlBase {
  protected readonly controlKind: NpStandardControlKind = 'input';
}

@Component({
  selector: 'np-textarea-control',
  imports: STANDARD_FORM_CONTROL_IMPORTS,
  templateUrl: './standard-form-controls.html',
  styleUrl: './standard-form-controls.scss',
})
export class NpTextareaControl extends NpStandardControlBase {
  protected readonly controlKind: NpStandardControlKind = 'textarea';
}

@Component({
  selector: 'np-select-control',
  imports: STANDARD_FORM_CONTROL_IMPORTS,
  templateUrl: './standard-form-controls.html',
  styleUrl: './standard-form-controls.scss',
})
export class NpSelectControl extends NpStandardControlBase {
  protected readonly controlKind: NpStandardControlKind = 'select';
}

@Component({
  selector: 'np-checkbox-control',
  imports: STANDARD_FORM_CONTROL_IMPORTS,
  templateUrl: './standard-form-controls.html',
  styleUrl: './standard-form-controls.scss',
})
export class NpCheckboxControl extends NpStandardControlBase {
  protected readonly controlKind: NpStandardControlKind = 'checkbox';
}

@Component({
  selector: 'np-radio-group-control',
  imports: STANDARD_FORM_CONTROL_IMPORTS,
  templateUrl: './standard-form-controls.html',
  styleUrl: './standard-form-controls.scss',
})
export class NpRadioGroupControl extends NpStandardControlBase {
  protected readonly controlKind: NpStandardControlKind = 'radio';
}

@Component({
  selector: 'np-date-control',
  imports: STANDARD_FORM_CONTROL_IMPORTS,
  templateUrl: './standard-form-controls.html',
  styleUrl: './standard-form-controls.scss',
})
export class NpDateControl extends NpStandardControlBase {
  protected readonly controlKind: NpStandardControlKind = 'date';
}
