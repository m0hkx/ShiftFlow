import { Component, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  imports: [],
  selector: 'app-employees',
  styleUrl: './employees.scss',
  templateUrl: './employees.html',
})
export class Employees {
  private http = inject(HttpClient);
  protected readonly employees = signal<Employee[]>([]);

  constructor() {
    this.http.get<Employee[]>('/api/employees').subscribe(list => this.employees.set(list));
  }

}
