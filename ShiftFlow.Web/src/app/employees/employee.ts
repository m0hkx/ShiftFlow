import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';

import type { Employee } from './employee.models';

@Service()
export class EmployeeService {
  private http = inject(HttpClient);

  getAll() {
    return this.http.get<Employee[]>('/api/employees');
  }
}
