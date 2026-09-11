import { Component, ElementRef, HostListener, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface SelectOption {
  value: string | number | null;
  label: string;
}

@Component({
  selector: 'app-select',
  templateUrl: './select.html',
  host: { class: 'relative inline-block' },
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: Select,
      multi: true,
    },
  ],
})
export class Select implements ControlValueAccessor {
  options = input.required<readonly SelectOption[]>();

  protected isOpen = signal(false);
  protected value = signal<string | number | null>(null);
  protected isDisabled = signal(false);

  private onChange: (value: string | number | null) => void = () => {};
  private onTouched: () => void = () => {};

  private elementRef = inject(ElementRef<HTMLElement>);

  protected get selectedLabel(): string {
    return this.options().find((o) => o.value === this.value())?.label ?? '';
  }

  writeValue(value: string | number | null): void {
    this.value.set(value);
  }

  registerOnChange(fn: (value: string | number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);
  }

  toggle(): void {
    if (this.isDisabled()) return;
    this.isOpen.update((open) => !open);
  }

  selectOption(option: SelectOption): void {
    this.value.set(option.value);
    this.onChange(option.value);
    this.onTouched();
    this.isOpen.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isOpen() && !this.elementRef.nativeElement.contains(event.target as Node)) {
      this.isOpen.set(false);
    }
  }
}
