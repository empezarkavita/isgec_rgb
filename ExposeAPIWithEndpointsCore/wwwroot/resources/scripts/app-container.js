const containerList = document.querySelector('#container-data');



// create element & render cafe
function renderContainer(doc){
    let tr = document.createElement('tr');
    let containerno = document.createElement('td');
    let source = document.createElement('td');
    let snapshot = document.createElement('td');
    let captureDate = document.createElement('td');


    tr.setAttribute('data-id', doc.containerno);
    containerno.textContent = doc.data().containerno;
    source.textContent = doc.data().source;
    snapshot.textContent = doc.data().snapshot;
    captureDate.textContent = doc.data().captureDate;

    tr.appendChild(containerno);
    tr.appendChild(source);
    tr.appendChild(snapshot);
    tr.appendChild(captureDate);

    containerList.appendChild(tr);

    // deleting data
    // cross.addEventListener('click', (e) => {
    //     e.stopPropagation();
    //     let id = e.target.parentElement.getAttribute('data-id');
    //     db.collection('cafes').doc(id).delete();
    // });
}

// getting data
// db.collection('scanned-containers').orderBy('captureDate').get().then(snapshot => {
//     snapshot.docs.forEach(doc => {
//         renderContainer(doc);
//     });
// });

// saving data
// form.addEventListener('submit', (e) => {
//     e.preventDefault();
//     db.collection('yard-containers').add({
//         name: form.name.value,
//         city: form.city.value
//     });
//     form.name.value = '';
//     form.city.value = '';
// });

// real-time listener
db.collection('scanned-containers').onSnapshot(snapshot => {
    let changes = snapshot.docChanges();
    changes.forEach(change => {
        console.log(change.doc.data());
        if(change.type == 'added'){
            renderContainer(change.doc);
        } else if (change.type == 'removed'){
            let tr = containerList.querySelector('[data-id=' + change.doc.containerno + ']');
            containerList.removeChild(tr);
        }
    });
});

// updating records (console demo)
// db.collection('cafes').doc('DOgwUvtEQbjZohQNIeMr').update({
//     name: 'mario world'
// });

// db.collection('cafes').doc('DOgwUvtEQbjZohQNIeMr').update({
//     city: 'hong kong'
// });

// setting data
// db.collection('cafes').doc('DOgwUvtEQbjZohQNIeMr').set({
//     city: 'hong kong'
// });