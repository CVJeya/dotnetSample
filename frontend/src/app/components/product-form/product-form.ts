import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { ProductService } from '../../services/product.service';

@Component({
  selector: 'app-product-form',
  imports: [FormsModule, RouterLink],
  template: `
    <div class="auth-container">
      <h1>{{ isEditMode ? 'Edit Product' : 'Add Product' }}</h1>

      @if (error()) {
        <div class="alert alert-error">{{ error() }}</div>
      }

      <form (ngSubmit)="submit()" #prodForm="ngForm" class="auth-form">
        <div class="form-group">
          <label for="name">Name</label>
          <input id="name" name="name" [(ngModel)]="name" required placeholder="Product name" />
        </div>
        <div class="form-group">
          <label for="description">Description</label>
          <textarea id="description" name="description" [(ngModel)]="description" required placeholder="Product description" rows="3"></textarea>
        </div>
        <div class="form-group">
          <label for="price">Price ($)</label>
          <input id="price" name="price" type="number" step="0.01" min="0" [(ngModel)]="price" required placeholder="0.00" />
        </div>
        <div class="form-actions">
          <button type="submit" class="btn btn-primary" [disabled]="saving() || prodForm.invalid">
            {{ saving() ? 'Saving...' : (isEditMode ? 'Update' : 'Create') }}
          </button>
          <a routerLink="/products" class="btn btn-secondary">Cancel</a>
        </div>
      </form>
    </div>
  `
})
export class ProductFormComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly name = signal('');
  readonly description = signal('');
  readonly price = signal<number | null>(null);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);

  isEditMode = false;
  private productId: number | null = null;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.isEditMode = true;
    this.productId = Number(id);

    this.productService.getProductById(this.productId).subscribe({
      next: (p) => {
        this.name.set(p.name);
        this.description.set(p.description);
        this.price.set(p.price);
      },
      error: () => this.error.set('Failed to load product')
    });
  }

  submit(): void {
    this.saving.set(true);
    this.error.set(null);
    const data = {
      name: this.name(),
      description: this.description(),
      price: Number(this.price() ?? 0)
    };

    const request$: Observable<unknown> = this.isEditMode
      ? this.productService.updateProduct(this.productId!, data)
      : this.productService.createProduct(data);

    request$.subscribe({
      next: () => this.router.navigate(['/products']),
      error: () => {
        this.error.set(this.isEditMode ? 'Failed to update product' : 'Failed to create product');
        this.saving.set(false);
      }
    });
  }
}
