import { Component, OnInit } from '@angular/core';
import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from '../_models/pagination';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {

  constructor(private userService: UserService, private alertify: AlertifyService,
    private route: ActivatedRoute) { }

  users: User[];
  pagination: Pagination;
  likesParam: string;

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['users'].result;
      this.pagination = data['users'].pagination;
    });

    this.likesParam = 'likers';
  }

  pageChanged(event: any) {
    this.pagination.currentPage = event.page;
    this.loadUsers();
  }

  loadLikers() {
    this.likesParam = 'likers';
    this.loadUsers();
  }

  loadLikees() {
    this.likesParam = 'likees';
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, null, this.likesParam)
      .subscribe((result: PaginatedResult<User[]>) => {
        this.users = result.result;
        this.pagination = result.pagination;
      }, error => {
        this.alertify.error(error);
      });
  }
}
