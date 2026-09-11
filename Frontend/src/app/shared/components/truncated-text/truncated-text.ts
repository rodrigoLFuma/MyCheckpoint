import { Component, input } from '@angular/core';

const DEFAULT_LIMIT = 30;

@Component({
  selector: 'app-truncated-text',
  templateUrl: './truncated-text.html',
})
export class TruncatedText {
  text = input<string | null | undefined>(null);
  limit = input<number>(DEFAULT_LIMIT);

  protected get visibleText(): string {
    const text = this.text() ?? '';
    if (text.length <= this.limit()) return text;
    return text.slice(0, this.limit()) + '…';
  }
}
