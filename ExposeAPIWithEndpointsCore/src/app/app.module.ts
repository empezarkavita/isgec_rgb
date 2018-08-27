import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { AngularFireModule } from 'angularfire2';
import { AngularFirestoreModule } from 'angularfire2/firestore';
import { FormsModule } from '@angular/forms';

var firebaseConfig = {
  apiKey: "AIzaSyB0zjjAJGGkFnUrlgKQvDtgIYSGoZHGKKE",
  authDomain: "project-rgb-8814c.firebaseapp.com",
  databaseURL: "https://project-rgb-8814c.firebaseio.com",
  projectId: "project-rgb-8814c",
  storageBucket: "project-rgb-8814c.appspot.com",
  messagingSenderId: "903850771479"
};

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AngularFireModule.initializeApp(firebaseConfig), 
    AngularFirestoreModule,
    FormsModule                            
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
