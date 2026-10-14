import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Auth } from '../auth/auth';
import { EmployeeService } from './employee';
import type { Employee } from './employee.models';

@Component({
  imports: [RouterLink],
  selector: 'app-employees',
  styleUrl: './employees.scss',
  templateUrl: './employees.html',
})
export class Employees {
  private employeeService = inject(EmployeeService);
  protected readonly auth = inject(Auth);

  protected readonly employees = signal<Employee[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.employeeService.getAll().subscribe({
      next: list => {
        this.employees.set(list);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not load employees.');
        this.loading.set(false);
      },
    });
  }
}
