import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from '../../_models/user';
import { ActivatedRoute } from '../../../../node_modules/@angular/router';
import { AlertifyService } from '../../_services/alertify.service';
import { NgForm } from '../../../../node_modules/@angular/forms';
import { UserService } from '../../_services/user.service';
import { AuthService } from '../../_services/auth.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  user: User;
  photoUrl: string;

  @ViewChild('editForm') editForm: NgForm;
  @HostListener('window:beforeunload', ['$event'])

  unloadNotification($event: any) {
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(private route: ActivatedRoute, private alertify: AlertifyService,
  private userService: UserService, private authSerivce: AuthService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data['user'];
      this.authSerivce.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
    });
  }

  updateUser() {

    const userId = +this.authSerivce.decodedToken.nameid;

    this.userService.updateUser(userId, this.user).subscribe(next => {
      this.alertify.success('Profile updated successfully!');
      this.editForm.reset(this.user);
    }, error => {
      this.alertify.error(error);
    });
  }

  updateMainPhoto(photoUrl) {
    this.user.photoUrl = photoUrl;
  }
}
