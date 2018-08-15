import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '../../../node_modules/@angular/router';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {

  model: any = {};
  photoUrl: string;

  constructor(public authService: AuthService, private alertService: AlertifyService,
  private router: Router) { }

  ngOnInit() {
    this.authService.currentPhotoUrl.subscribe(photoUrl => {
      this.photoUrl = photoUrl;
    });
  }

  login() {
    this.authService.login(this.model).subscribe(next => {
      this.model.username = this.authService.decodedToken.unique_name;
      this.alertService.success('Logged In!');
    }, error => {
      this.alertService.error(error);
    }, () => {
      this.router.navigate(['/members']);
    } );
  }

  loggedin() {
    return this.authService.loggedin();
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.authService.decodedToken = null;
    this.authService.currentUser = null;
    this.alertService.message('Logged Out!');
    this.router.navigate(['/home']);
  }
}
