import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-product-form',
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <h1>{{ isEditMode ? 'Edit Product' : 'Add Product' }}</h1>

      <div *ngIf="error" class="alert alert-danger">{{ error }}</div>

      <form (ngSubmit)="submit()" #prodForm="ngForm" class="auth-form">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" name="name" [(ngModel)]="name" required class="form-control" placeholder="Product name" />
        </div>
        <div class="form-group">
          <label for="description">Description</label>
          <textarea id="description" name="description" [(ngModel)]="description" required class="form-control" placeholder="Product description" rows="3"></textarea>
        </div>
        <div class="form-group">
          <label for="price">Price ($)</label>
          <input id="price" name="price" type="number" step="0.01" [(ngModel)]="price" required min="0" class="form-control" placeholder="0.00" />
        </div>
        <div class="form-actions">
          <button type="submit" [disabled]="saving || prodForm.invalid" class="btn-primary">
            {{ saving ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
          </button>
          <a routerLink="/products" class="btn-cancel">Cancel</a>
        </div>
      </form>
    </div>
  `
})
export class ProductFormComponent {
  private readonly productService = inject(ProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  name = '';
  description = '';
  price = 0;
  isEditMode = false;
  productId: number | null = null;
  saving = false;
  error: string | null = null;

  ngOnInit(): void {
    console.log('[ProductFormComponent] Component initialized');
    const id = this.route.snapshot.paramMap.get('id');
    console.log('[ProductFormComponent] Route param id:', id);
    
    if (id) {
      this.isEditMode = true;
      this.productId = Number(id);
      console.log('[ProductFormComponent] Edit mode enabled - loading product with id:', this.productId);
      
      this.productService.getProductById(this.productId).subscribe({
        next: (p) => {
          console.log('[ProductFormComponent] ✓ Product loaded:', p);
          this.name = p.name;
          this.description = p.description;
          this.price = p.price;
        },
        error: (err) => {
          console.log('[ProductFormComponent] ✗ Failed to load product:', err);
          this.error = 'Failed to load product';
        }
      });
    } else {
      console.log('[ProductFormComponent] Create mode enabled');
    }
  }

  submit(): void {
    console.log('[ProductFormComponent] submit() called - isEditMode:', this.isEditMode);
    this.saving = true;
    this.error = null;
    const data = { name: this.name, description: this.description, price: this.price };
    console.log('[ProductFormComponent] Submitting data:', data);

    if (this.isEditMode) {
      console.log('[ProductFormComponent] Updating product with id:', this.productId);
      this.productService.updateProduct(this.productId!, data).subscribe({
        next: () => {
          console.log('[ProductFormComponent] ✓ Product updated - navigating to /products');
          this.router.navigate(['/products']);
        },
        error: (err) => {
          console.log('[ProductFormComponent] ✗ Failed to update product:', err);
          this.error = 'Failed to update product';
          this.saving = false;
        }
      });
    } else {
      console.log('[ProductFormComponent] Creating new product');
      this.productService.createProduct(data).subscribe({
        next: () => {
          console.log('[ProductFormComponent] ✓ Product created - navigating to /products');
          this.router.navigate(['/products']);
        },
        error: (err) => {
          console.log('[ProductFormComponent] ✗ Failed to create product:', err);
          this.error = 'Failed to create product';
          this.saving = false;
        }
      });
    }
  }
}
