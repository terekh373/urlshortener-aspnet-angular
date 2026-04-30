import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShortenedUrl } from '../../models/url.model';
import { UrlService } from '../../core/services/url.service';
import { AuthService } from '../../core/services/auth.service';
import { AddUrlComponent } from '../add-url/add-url.component';

@Component({
  selector: 'app-url-table',
  standalone: true,
  imports: [CommonModule, AddUrlComponent, FormsModule],
  templateUrl: './url-table.component.html'
})
export class UrlTableComponent implements OnInit {
  urls: ShortenedUrl[] = [];
  errorMessage = '';
  successMessage = '';
  copiedId: number | null = null;

  searchQuery = '';

  sortColumn: keyof ShortenedUrl = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';

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

  // Copy to clipboard
  copyToClipboard(url: ShortenedUrl): void {
    navigator.clipboard.writeText(url.shortUrl).then(() => {
      this.copiedId = url.id;
      setTimeout(() => this.copiedId = null, 2000);
    });
  }

  sortBy(column: keyof ShortenedUrl): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
  }

  // returns icon header
  getSortIcon(column: keyof ShortenedUrl): string {
    if (this.sortColumn !== column) return '↕';
    return this.sortDirection === 'asc' ? '↑' : '↓';
  }

  // returns filtered and sorted array
  get filteredAndSortedUrls(): ShortenedUrl[] {
    let result = [...this.urls];

    if (this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase();
      result = result.filter(u =>
        u.originalUrl.toLowerCase().includes(query) ||
        u.shortCode.toLowerCase().includes(query) ||
        u.createdBy?.toLowerCase().includes(query)
      );
    }

    result.sort((a, b) => {
      const valA = a[this.sortColumn];
      const valB = b[this.sortColumn];

      if (valA == null) return 1;
      if (valB == null) return -1;

      let comparison = 0;
      if (typeof valA === 'string' && typeof valB === 'string') {
        comparison = valA.localeCompare(valB);
      } else {
        comparison = valA < valB ? -1 : valA > valB ? 1 : 0;
      }

      return this.sortDirection === 'asc' ? comparison : -comparison;
    });

    return result;
  }
}