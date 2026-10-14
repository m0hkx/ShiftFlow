import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { EmployeeService } from '../employee';

@Component({
  imports: [ReactiveFormsModule, RouterLink],
  selector: 'app-employee-form',
  styleUrl: './employee-form.scss',
  templateUrl: './employee-form.html',
})
export class EmployeeForm {
  private employeeService = inject(EmployeeService);
  private router = inject(Router);

  // Same rules as CreateEmployeeDto on the API. The server stays the source of truth.
  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    position: ['', Validators.required],
    team: [''],
    active: [true],
  });

  protected readonly error = signal<string | null>(null);
  protected readonly submitting = signal(false);

  protected submit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.error.set(null);
    this.submitting.set(true);

    const value = this.form.getRawValue();

    // An empty team is sent as null, which is what the API expects for string?
    this.employeeService.create({ ...value, team: value.team || null }).subscribe({
      next: () => this.router.navigate(['/employees']),
      error: err => {
        this.submitting.set(false);
        this.error.set(
          err.status === 403 ? 'Only managers can add employees.'
          : err.status === 400 ? 'Some fields are invalid. Check the form and try again.'
          : 'Could not save the employee. Try again.'
        );
      },
    });
  }
}
