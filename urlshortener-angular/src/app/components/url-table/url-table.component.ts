import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShortenedUrl } from '../../models/url.model';
import { UrlService } from '../../core/services/url.service';
import { AuthService } from '../../core/services/auth.service';
import { AddUrlComponent } from '../add-url/add-url.component';

@Component({
  selector: 'app-url-table',
  standalone: true,
  imports: [CommonModule, AddUrlComponent],
  templateUrl: './url-table.component.html'
})
export class UrlTableComponent implements OnInit {
  urls: ShortenedUrl[] = [];
  errorMessage = '';
  successMessage = '';

  constructor(
    public urlService: UrlService,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadUrls();
  }

  loadUrls(): void {
    this.urlService.getAll().subscribe({
      next: (data) => this.urls = data,
      error: () => this.errorMessage = 'Failed to load URLs'
    });
  }

  onUrlAdded(url: ShortenedUrl): void {
    this.urls = [...this.urls, url];
    this.successMessage = 'URL added successfully!';
    setTimeout(() => this.successMessage = '', 3000);
  }

  onDelete(id: number): void {
    this.urlService.delete(id).subscribe({
      next: () => {
        this.urls = this.urls.filter(u => u.id !== id);
      },
      error: () => this.errorMessage = 'Failed to delete URL'
    });
  }

  canDelete(url: ShortenedUrl): boolean {
    return this.auth.isAdmin() ||
           url.createdBy === this.auth.getUsername();
  }
}