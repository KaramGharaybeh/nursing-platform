import { TestBed } from '@angular/core/testing';
import { AuthTextField } from './auth-text-field';

describe('AuthTextField', () => {
  it('renders a persistent label above a native input with associated support and error text', async () => {
    await TestBed.configureTestingModule({ imports: [AuthTextField] }).compileComponents();
    const fixture = TestBed.createComponent(AuthTextField);
    fixture.componentRef.setInput('controlId', 'email');
    fixture.componentRef.setInput('label', 'Email address');
    fixture.componentRef.setInput('type', 'email');
    fixture.componentRef.setInput('required', true);
    fixture.componentRef.setInput('helperText', 'Enter your email address.');
    fixture.componentRef.setInput('errorText', 'Email is required.');
    fixture.detectChanges();

    const root = fixture.nativeElement as HTMLElement;
    const input = root.querySelector<HTMLInputElement>('#email');
    expect(root.querySelector('mat-form-field')).toBeNull();
    expect(root.querySelector('label[for="email"]')?.textContent).toContain('Email address');
    expect(root.querySelector('.np-auth-field-label + .np-auth-field-control input')?.id).toBe('email');
    expect(input?.required).toBe(true);
    expect(input?.getAttribute('aria-invalid')).toBe('true');
    expect(input?.getAttribute('aria-describedby')).toBe('email-helper email-error');
    expect(root.querySelector('#email-helper')?.textContent).toContain('Enter your email address.');
    expect(root.querySelector('#email-error')?.textContent).toContain('Email is required.');
  });

  it('emits input values without changing masking or disabled semantics', async () => {
    await TestBed.configureTestingModule({ imports: [AuthTextField] }).compileComponents();
    const fixture = TestBed.createComponent(AuthTextField);
    fixture.componentRef.setInput('controlId', 'password');
    fixture.componentRef.setInput('label', 'Password');
    fixture.componentRef.setInput('type', 'password');
    fixture.componentRef.setInput('disabled', true);
    const valueChange = vi.fn();
    fixture.componentInstance.valueChange.subscribe(valueChange);
    fixture.detectChanges();

    const input = (fixture.nativeElement as HTMLElement).querySelector<HTMLInputElement>('#password');
    expect(input?.type).toBe('password');
    expect(input?.disabled).toBe(true);
    fixture.componentRef.setInput('disabled', false);
    fixture.detectChanges();
    if (input === null) throw new Error('Missing password field');
    input.value = 'entered-secret';
    input.dispatchEvent(new Event('input'));
    expect(valueChange).toHaveBeenCalledWith('entered-secret');
  });

  it('toggles the password without submitting or losing its value', async () => {
    await TestBed.configureTestingModule({ imports: [AuthTextField] }).compileComponents();
    const fixture = TestBed.createComponent(AuthTextField);
    fixture.componentRef.setInput('controlId', 'password');
    fixture.componentRef.setInput('label', 'Password');
    fixture.componentRef.setInput('type', 'password');
    fixture.componentRef.setInput('value', 'SecurePass1');
    fixture.componentRef.setInput('showVisibilityToggle', true);
    fixture.detectChanges();
    const root = fixture.nativeElement as HTMLElement;
    const input = root.querySelector<HTMLInputElement>('#password');
    const button = root.querySelector<HTMLButtonElement>('button');
    expect(button?.type).toBe('button');
    expect(button?.getAttribute('aria-label')).toBe('Show password');
    expect(input?.type).toBe('password');
    expect(button?.querySelector('svg[data-icon="visibility"]')).not.toBeNull();
    expect(button?.querySelector('svg[data-icon="visibility-off"]')).toBeNull();
    button?.click();
    fixture.detectChanges();
    expect(input?.type).toBe('text');
    expect(input?.value).toBe('SecurePass1');
    expect(button?.getAttribute('aria-label')).toBe('Hide password');
    expect(button?.querySelector('svg[data-icon="visibility-off"]')).not.toBeNull();
    expect(button?.querySelector('svg[data-icon="visibility"]')).toBeNull();
    button?.click();
    fixture.detectChanges();
    expect(input?.type).toBe('password');
    expect(input?.value).toBe('SecurePass1');
    expect(button?.getAttribute('aria-label')).toBe('Show password');
    expect(button?.querySelector('svg[data-icon="visibility"]')).not.toBeNull();
  });
});
