import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment.prod';
import { HttpClient, HttpHeaders } from '../../../node_modules/@angular/common/http';
import { Observable } from '../../../node_modules/rxjs';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.baseUrl + 'users');
  }

  getUser(id): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put<User>(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(userid: number, photoid: number) {
    return this
      .http
      .post(this.baseUrl + 'users/' + userid + '/photos/' + photoid + '/setMain' , {});
  }

  deletePhoto(userid: number, photoid: number) {
    return this
      .http
      .delete(this.baseUrl + 'users/' + userid + '/photos/' + photoid + '/delete' , {});
  }
}
