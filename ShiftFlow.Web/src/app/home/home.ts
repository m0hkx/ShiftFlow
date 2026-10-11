import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { Auth } from '../auth/auth';

interface Employee {
  id: string;
  name: string;
  email: string;
  position: string;
  team: string | null;
  active: boolean;
}

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
})
export class Home {
  private http = inject(HttpClient);
  private router = inject(Router);
  protected readonly auth = inject(Auth);

  protected readonly employees = signal<Employee[]>([]);

  constructor() {
    this.http.get<Employee[]>('/api/employees').subscribe(list => this.employees.set(list));
  }

  protected logout() {
    this.auth.logout().subscribe({
      complete: () => this.router.navigate(['/login']),
      error: () => this.router.navigate(['/login']),
    });
  }
}
