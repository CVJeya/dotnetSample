import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from '../models/product';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/products`;

  getProducts(): Observable<Product[]> {
    console.log('[ProductService] getProducts() called');
    return this.http.get<Product[]>(this.apiUrl);
  }

  getProductById(id: number): Observable<Product> {
    console.log('[ProductService] getProductById() called with id:', id);
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  createProduct(product: Omit<Product, 'id' | 'createdBy' | 'createdAt'>): Observable<Product> {
    console.log('[ProductService] createProduct() called with product:', product);
    return this.http.post<Product>(this.apiUrl, product);
  }

  updateProduct(id: number, product: Omit<Product, 'id' | 'createdBy' | 'createdAt'>): Observable<void> {
    console.log('[ProductService] updateProduct() called with id:', id, 'product:', product);
    return this.http.put<void>(`${this.apiUrl}/${id}`, product);
  }

  deleteProduct(id: number): Observable<void> {
    console.log('[ProductService] deleteProduct() called with id:', id);
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
