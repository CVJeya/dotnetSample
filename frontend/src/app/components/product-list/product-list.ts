import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ProductService } from '../../services/product.service';
import { Product } from '../../models/product';

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, RouterLink],
  template: `
    <div class="container">
      <h1>Products</h1>

      <ng-container *ngIf="products$ | async as products">
        <table *ngIf="products.length > 0" class="table">
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
            <tr *ngFor="let product of products">
              <td>{{ product.id }}</td>
              <td>{{ product.name }}</td>
              <td>{{ product.description }}</td>
              <td>{{ product.price | currency }}</td>
              <td>{{ product.createdBy }}</td>
              <td>
                <button (click)="deleteProduct(product.id)" class="btn-delete">Delete</button>
              </td>
            </tr>
          </tbody>
        </table>

        <p *ngIf="products.length === 0" class="text-muted">No products found.</p>
      </ng-container>

      <a routerLink="/products/new" class="btn-primary">Add Product</a>
    </div>
  `
})
export class ProductListComponent {
  private readonly productService = inject(ProductService);
  products$!: Observable<Product[]>;

  ngOnInit(): void {
    console.log('[ProductListComponent] Component initialized - fetching products');
    this.products$ = this.productService.getProducts();
    this.products$.subscribe({
      next: (products) => console.log('[ProductListComponent] ✓ Products loaded:', products),
      error: (err) => console.log('[ProductListComponent] ✗ Failed to load products:', err)
    });
  }

  deleteProduct(id: number): void {
    console.log('[ProductListComponent] deleteProduct() called with id:', id);
    if (!confirm('Delete this product?')) {
      console.log('[ProductListComponent] Delete cancelled by user');
      return;
    }
    
    console.log('[ProductListComponent] User confirmed delete - calling productService.deleteProduct()');
    this.productService.deleteProduct(id).subscribe({
      next: () => {
        console.log('[ProductListComponent] ✓ Product deleted - refreshing list');
        this.products$ = this.productService.getProducts();
      },
      error: (err) => {
        console.log('[ProductListComponent] ✗ Failed to delete product:', err);
      }
    });
  }
}
