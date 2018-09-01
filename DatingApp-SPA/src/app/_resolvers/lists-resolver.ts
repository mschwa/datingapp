import { Injectable } from '../../../node_modules/@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from '../../../node_modules/rxjs';
import { catchError } from '../../../node_modules/rxjs/operators';

@Injectable()
export class ListsResolver implements Resolve<User[]> {

    constructor(private userService: UserService, private router: Router,
    private alertify: AlertifyService) {}

    pageNumber = 1;
    pageSize = 5;
    likesParam = 'likers';

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
       return this.userService.getUsers(this.pageNumber, this.pageSize, null, this.likesParam).pipe(
            catchError(error => {
                this.alertify.error('Problem Retrieving Data');
                this.router.navigate(['/home']);
                return of(null);
            })
        );
    }
}
