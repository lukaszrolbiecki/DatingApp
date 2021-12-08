import { AccountService } from './../_services/account.service';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { User } from '../_models/user';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
})
export class RegisterComponent implements OnInit {
  @Output() cancelEvent = new EventEmitter();
  username: string;
  model: any = {};

  constructor(private accountService: AccountService) {}

  ngOnInit(): void {}

  register() {
    console.log(this.model);
    this.accountService.register(this.model).subscribe((user) => {
      console.log(user);
      this.cancelRegistration();
    });
  }

  cancelRegistration() {
    this.cancelEvent.emit(false);
  }
}
