import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler
} from '@angular/common/http';


@Injectable()
export class AuthInterceptorService implements HttpInterceptor {
  constructor() {}

  intercept(req: HttpRequest<any>, next: HttpHandler) {
    let userToken = localStorage.getItem('auth-token');
    let isTp = req.url.indexOf('positionstack') > -1;
    if(userToken && !isTp){
        const modifiedReq = req.clone({
          headers: req.headers.set('Authorization', `Bearer ${userToken}`)
        });
        
        return next.handle(modifiedReq);
    }else{
        return next.handle(req);
    }
  }
}
