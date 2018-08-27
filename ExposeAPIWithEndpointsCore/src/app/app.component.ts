import { Component } from '@angular/core';
import { AngularFirestore, AngularFirestoreCollection, AngularFirestoreDocument } from 'angularfire2/firestore';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {

  containerCol: AngularFirestoreCollection<Container>;
  containers: Observable<Container[]>;
  color: string;
  containerno: string;
  footer: string;
  party: string;
  status: string;
  constructor(private afs: AngularFirestore) {

  }

  ngOnInit() {

    this.containerCol = this.afs.collection('msc-mnr');
    this.containers = this.containerCol.valueChanges();
  }

  title = 'App';
}

interface Container {
  color: string;
  containerno: string;
  footer: string;
  party: string;
  status: string;
}