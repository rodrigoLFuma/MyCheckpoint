import { Component, computed, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'app-rating-picker',
  templateUrl: './rating-picker.html',
  host: { class: 'inline-block' },
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: RatingPicker,
      multi: true,
    },
  ],
})
export class RatingPicker implements ControlValueAccessor {
  max = input<number>(10);

  protected numbers = computed(() => Array.from({ length: this.max() + 1 }, (_, i) => i));

  protected value = signal(0);
  protected hovered = signal<number | null>(null);
  protected isDisabled = signal(false);

  protected displayValue = computed(() => this.hovered() ?? this.value());

  private onChange: (value: number) => void = () => {};
  private onTouched: () => void = () => {};

  writeValue(value: number): void {
    this.value.set(value ?? 0);
  }

  registerOnChange(fn: (value: number) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);
  }

  onHover(n: number): void {
    if (this.isDisabled()) return;
    this.hovered.set(n);
  }

  onLeave(): void {
    this.hovered.set(null);
  }

  onClick(n: number): void {
    if (this.isDisabled()) return;
    this.value.set(n);
    this.onChange(n);
    this.onTouched();
  }
}
