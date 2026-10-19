import { Component, inject, signal } from '@angular/core';
import { HomeService } from './home-service';

@Component({
  selector: 'app-home',
  templateUrl: './home.html',
})

export class Home {
  private homeService = inject(HomeService);
  
  protected readonly employeeNum = signal(0); 

  constructor() {
    this.homeService.getAll().subscribe({
      next: list => {
        this.employeeNum.set(list.length);
      },
      error: () => {
        console.log("Error");
      },
    });
  }
}
