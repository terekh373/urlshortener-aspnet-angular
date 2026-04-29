import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UrlService } from '../../core/services/url.service';
import { ShortenedUrl } from '../../models/url.model';

@Component({
  selector: 'app-add-url',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-url.component.html'
})
export class AddUrlComponent {
  @Output() urlAdded = new EventEmitter<ShortenedUrl>();

  originalUrl = '';
  errorMessage = '';
  isLoading = false;

  constructor(private urlService: UrlService) {}

  isValidUrl(url: string): boolean {
    try {
      const parsed = new URL(url);
      return parsed.protocol === 'http:' || parsed.protocol === 'https:';
    } catch {
      return false;
    }
  }

  onSubmit(): void {
    if (!this.originalUrl.trim()) {
      this.errorMessage = 'Please enter a URL';
      return;
    }

    if (!this.isValidUrl(this.originalUrl)) {
      this.errorMessage = 'Please enter a valid URL (must start with http:// or https://)';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.urlService.create(this.originalUrl).subscribe({
      next: (url) => {
        this.urlAdded.emit(url);
        this.originalUrl = '';
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Failed to add URL';
        this.isLoading = false;
      }
    });
  }
}