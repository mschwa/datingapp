import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from '../../_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../_services/auth.service';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input()
  photos: Photo[];
  @Output()
  getMemberPhotoChanges = new EventEmitter<string>();
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMain: Photo;

  constructor(
    private authService: AuthService,
    private userSerivce: UserService,
    private alertify: AlertifyService
  ) {}

  ngOnInit() {
    this.initializeUploader();
  }

  public fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url:
        this.baseUrl +
        'users/' +
        this.authService.decodedToken.nameid +
        '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = file => {
      file.withCredentials = false;
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };

        this.photos.push(photo);
      }
    };
  }

  setMainPhoto(photo: Photo) {
    this.userSerivce
      .setMainPhoto(this.authService.decodedToken.nameid, photo.id)
      .subscribe(
        next => {
          this.currentMain = this.photos.filter(p => p.isMain === true)[0];
          this.currentMain.isMain = false;
          photo.isMain = true;
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem(
            'user',
            JSON.stringify(this.authService.currentUser)
          );
        },
        error => {
          this.alertify.error(error);
        }
      );
  }

  deletePhoto(photoid: number) {
    this.alertify.confirm('Are you sure?', () => {
      this.userSerivce
        .deletePhoto(this.authService.decodedToken.nameid, photoid)
        .subscribe(() => {
          const indexToDelete = this.photos.findIndex(p => p.id === photoid);
          this.photos.splice(indexToDelete, 1);
          this.alertify.success('Photo deleted!');
        }, error => {
          this.alertify.error(error);
        });
    });
  }
}