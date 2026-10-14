import { HttpClient } from '@angular/common/http';
import { inject, Service } from '@angular/core';

import type { CreateEmployee, Employee } from './employee.models';

@Service()
export class EmployeeService {
  private http = inject(HttpClient);

  getAll() {
    return this.http.get<Employee[]>('/api/employees');
  }

  create(employee: CreateEmployee) {
    return this.http.post<Employee>('/api/employees', employee);
  }
}
