import { Component, OnInit, inject, signal } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product';

@Component({
  selector: 'app-product-list',
  imports: [CurrencyPipe, RouterLink],
  template: `
    @if (loading()) {
      <div class="loading">Loading products...</div>
    } @else if (error()) {
      <div class="error">Error: {{ error() }}</div>
    } @else {
      <div>
        <div class="page-header">
          <h1>Products</h1>
          <a routerLink="/products/new" class="btn btn-primary">Add Product</a>
        </div>

        @if (products().length === 0) {
          <div class="empty-state">No products found.</div>
        } @else {
          <div class="table-wrapper">
            <table class="user-table">
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Price</th>
                  <th>Created By</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (product of products(); track product.id) {
                  <tr>
                    <td>{{ product.id }}</td>
                    <td class="user-link">{{ product.name }}</td>
                    <td>{{ product.description }}</td>
                    <td>{{ product.price | currency: 'USD' }}</td>
                    <td>{{ product.createdBy }}</td>
                    <td>
                      <div class="actions">
                        <button type="button" class="btn btn-sm btn-danger" (click)="deleteProduct(product.id)">Delete</button>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
        }
      </div>
    }
  `
})
export class ProductListComponent implements OnInit {
  private readonly productService = inject(ProductService);

  readonly products = signal<Product[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.productService.getProducts().subscribe({
      next: (list) => {
        this.products.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Failed to fetch products');
        this.loading.set(false);
      }
    });
  }

  deleteProduct(id: number): void {
    if (!confirm('Delete this product?')) {
      return;
    }

    this.productService.deleteProduct(id).subscribe({
      next: () => this.products.update((list) => list.filter((p) => p.id !== id)),
      error: () => this.error.set('Failed to delete product')
    });
  }
}
