import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
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
    public auth: AuthService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadUrls();
    setInterval(() => this.loadUrls(), 10000);
  }

  loadUrls(): void {
    this.urlService.getAll().subscribe({
      next: (data) => {
        this.urls = [...data];
        this.cdr.detectChanges();
      },
      error: () => this.errorMessage = 'Failed to load URLs'
    });
  }

  onUrlAdded(url: ShortenedUrl): void {
    this.urls = [...this.urls, url];
    this.successMessage = 'URL added successfully!';
    this.cdr.detectChanges();
    setTimeout(() => this.successMessage = '', 3000);
  }

  onDelete(id: number): void {
    if (!confirm('Are you sure you want to delete this URL?')) return;
    this.urlService.delete(id).subscribe({
      next: () => {
        this.urls = this.urls.filter(u => u.id !== id);
        this.cdr.detectChanges();
      },
      error: () => this.errorMessage = 'Failed to delete URL'
    });
  }

  canDelete(url: ShortenedUrl): boolean {
    return this.auth.isAdmin() ||
      url.createdBy === this.auth.getUsername();
  }

  copyToClipboard(url: ShortenedUrl): void {
    navigator.clipboard.writeText(url.shortUrl).then(() => {
      this.copiedId = url.id;
      this.cdr.detectChanges();
      setTimeout(() => {
        this.copiedId = null;
        this.cdr.detectChanges();
      }, 2000);
    });
  }

  sortBy(column: keyof ShortenedUrl): void {
    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.cdr.detectChanges();
  }

  getSortIcon(column: keyof ShortenedUrl): string {
    if (this.sortColumn !== column) return '↕';
    return this.sortDirection === 'asc' ? '↑' : '↓';
  }

  getDetailsUrl(id: number): string {
    const token = this.auth.getToken();
    return `https://localhost:7150/urls/details/${id}?token=${token}`;
  }

  get filteredAndSortedUrls(): ShortenedUrl[] {
    let result = [...this.urls];

    const query = this.searchQuery.trim().toLowerCase();
    if (query) {
      result = result.filter(u =>
        u.originalUrl.toLowerCase().includes(query) ||
        u.shortCode.toLowerCase().includes(query) ||
        (u.createdBy?.toLowerCase() ?? '').includes(query)
      );
    }

    result.sort((a, b) => {
      const valA = a[this.sortColumn] ?? '';
      const valB = b[this.sortColumn] ?? '';

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