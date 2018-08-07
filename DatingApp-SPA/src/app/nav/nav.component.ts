import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {};

  constructor(public authService: AuthService, private alertService: AlertifyService) { }

  ngOnInit() {
  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.model.username = this.authService.decodedToken.unique_name;
      this.alertService.success('Logged In!');
    }, error => {
      this.alertService.error(error);
    });
  }

  loggedin() {
    return this.authService.loggedin();
  }

  logout() {
    localStorage.removeItem('token');
    this.alertService.message('Logged Out!');
  }
}
