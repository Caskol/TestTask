var categories = [], events = [];
var lastClickedDate;

function Category(id, name, hex_color) {
    this.id = id;
    this.name = name;
    this.hex_color = hex_color;
}

function DailyEvent(id, name, date, category) {
    this.id = id;
    this.name = name;
    this.date = date;
    this.category = category;
}
//Прослушивалка при загрузке страницы.
document.addEventListener("DOMContentLoaded", function () {
    console.log("Скрипт с другого файла работает, тестируй");
    //Когда страница загрузилась - начинаем обращаться к сервису чтобы получить данные
    $('#calendar').fullCalendar({
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month'
        },
        dayClick: function (date, jsEvent, view) {
            $('#modalDate').text('Выбрана дата: ' + date.format('YYYY-MM-DD'));
            // Сброс списка событий
            $('#eventList').empty();
            //добавляем в модальное окно конкретного дня html информацию о событиях
            addEventsFromArray(date);
            lastClickedDate=date;
            //отображаем окно
            $('#infoModal').modal('show');
        },
        events: updateData,
        viewRender: setBackgroundColorToCells
    });
});

function setBackgroundColorToCells() {
    let currentView = $('#calendar').fullCalendar('getView');
    let startDate = currentView.intervalStart;
    let endDate = currentView.intervalEnd;
    //console.log(startDate.format('MM'));
    //console.log(endDate.format('MM'));

    //получаем события в текущий месяц
    let eventsInThisMonth = events.filter((event) => event.date.substring(0, 10) >= startDate.format('YYYY-MM-DD') && event.date.substring(0, 10) < endDate.format('YYYY-MM-DD'))
    //console.log(eventsInThisMonth);
    eventsInThisMonth.forEach(function (event) {
        //console.log('[data-date="' + event.date.substring(0, 10) + '"]');
        //console.log(event);
        var cell = $('[data-date="' + event.date.substring(0, 10) + '"]');
        //console.log(event.category.hex_color);
        if (event.category != null)
        cell.css('background-color', event.category.hex_color);
    });
}

//Добавление события с массива (т.е. больше к сервису не обращаемся если информация уже загружена, нет смысла тревожить бд)
function addEventsFromArray(eventDate) {
    console.log(events.filter((ev) => { return ev.date === eventDate.format("YYYY-MM-DD") }));
    //ищем в массиве событие ПО ДАТЕ
    let eventsFromArray = events.filter((ev) => { return ev.date === eventDate.format("YYYY-MM-DD") });

    if (eventsFromArray.length > 0) {
        eventsFromArray.forEach(function (event) {
            $('#eventList').append('<input type="button" onclick="deleteEvent(' + event.id + ')" value="Удалить это событие"></input>');
            $('#eventList').append('<input type="button" onclick="editEvent(' + event.id + ')" value="Редактировать это событие"></input>');
            $('#eventList').append('<li>ID события: ' + event.id + '</li>');
            $('#eventList').append('<li>Название события: ' + event.name + '</li>');
            $('#eventList').append('<li>Дата события: ' + event.date.substring(0, 10) + '</li>');
            if (event.category != null) {
                $('#eventList').append('<li>Категория события: ' + event.category.name + '</li>');
                $('#eventList').append('<li>Цвет события ' + '<input type="color" value=' + event.category.hex_color + '></li>');
            }
            else {
                $('#eventList').append('<li>Категория события: Нет категории</li>');
            }
        });
    } else {
        $('#eventList').append('<li>Событий нет</li>');
    }
}

//events в календаре позволяет использовать только 1 метод, а загружать надо 2 класса
function updateData() {
    loadResource('/api/categories', loadCategories, 'Ошибка при загрузке категорий');
    loadResource('/api/events', loadEvents, 'Ошибка при загрузке событий');
}
//Просто вспомогательная функция для DRY
function loadResource(url, successCallback, errorCallback) {
    
    $.ajax({
        url: url,
        method: 'GET',
        dataType: 'json',
        success: successCallback,
        error: function () {
            alert(errorCallback);
        }
    });
}
//Функция для поиска категории в массиве по id
function findCategoryInArrayById(id) {
    return categories.find((category) => {
        return category.id === id;
    })
}

//Функция для поиска категории в массиве по id
function findCategoryInArrayByName(name) {
    return categories.find((category) => {
        return category.name === name;
    })
}
//Функция для добавления загруженных с сервиса данных в массив
function loadEvents(eventsFromQuery) {
    console.log("Ищу ресурсы - события");
    events = [];
    eventsFromQuery.forEach(function (eventFromApi) {
        if (eventFromApi.categoryId != null) {
            var category = new Category(eventFromApi.category.id, eventFromApi.category.name, eventFromApi.category.colorInHex);
        }
        else {
            var category = null;
        }
        let event = new DailyEvent(eventFromApi.id, eventFromApi.name, eventFromApi.eventDate.substring(0, 10), category);
        events.push(event);
    });
    setBackgroundColorToCells(); //обновляем ячейки после каждого поиска событий
}
//Функция для добавления загруженных с сервиса данных в массив
function loadCategories(categoriesFromQuery) {
    console.log("Ищу ресурсы - категории");
    // Обработка полученных категорий
    categories = [];
    categoriesFromQuery.forEach(function (categoryFromApi) {
        let category = new Category(categoryFromApi.id, categoryFromApi.name, categoryFromApi.colorInHex);
        categories.push(category);
    });
}
//Отобразить окно добавления информации
function displayNewEventWindow() {
    $('#insertBody').empty();
    $('#insertBody').append('<input type="text" placeholder="Название события" id="eventName"/>');
    categories.forEach(function (category) {
        $('#insertBody').append('<div>')
        $('#insertBody').append('<input type="radio" value="' + category.name + '" id="' + category.name + '" name="category"/>');
        $('#insertBody').append('<label for="' + category.name + '">' + category.name + '</label>');
        $('#insertBody').append('</div>')
    });
    $('#insertBody').append('<div>')
    $('#insertBody').append('<input type="radio" value="Без категории" id="null" name="category"/>');
    $('#insertBody').append('<label for="Без категории">Без категории</label>');
    $('#insertBody').append('</div>');
    $('#addNewEvent').modal('show');
}

function createNewEvent() {
    let eventName = $('#eventName').val();
    console.log(eventName);
    let category = $('input[type="radio"]:checked').attr('id');
    console.log(category);
    var dataToTransfer;
    if (category != 'null') {
        dataToTransfer = {
            "name": eventName,
            "eventDate": lastClickedDate.format('YYYY-MM-DD'),
            "categoryId": findCategoryInArrayByName(category).id
        };
    }
    else {
        dataToTransfer = {
            "name": eventName,
            "eventDate": lastClickedDate.format('YYYY-MM-DD'),
            "categoryId": null
        };
    }
    console.log(dataToTransfer);
    $.ajax({
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        url: '/api/events',
        method: 'POST',
        dataType: 'json',
        data: JSON.stringify(dataToTransfer),
        success: () => {
            updateData();
            alert('Добавлено в базу данных');

        },
        error: () => {
            alert('Не удалось добавить событие в базу данных');
        }
    });
    
}



