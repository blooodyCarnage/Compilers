# Лабораторная работа №5 — Построение AST и проверка контекстно-зависимых условий
## Цель работы
Изучить назначение и принципы работы семантического анализатора в структуре компилятора. Освоить методы построения абстрактного синтаксического дерева (AST) и проверки контекстно-зависимых условий (семантических правил) для заданной синтаксической конструкции.
## Автор
+ Марченко А.Е.
+ Группа: АП-326
## Вариант задания
Обрабатываемая конструкция
```
DECLARE product_price CONSTANT INTEGER := 150;

```
## Примеры корректных строк
```
DECLARE product_price CONSTANT INTEGER := 150;
DECLARE product CONSTANT INTEGER := -25000;
DECLARE price CONSTANT INTEGER := product_price;
```
## CST / AST (схема для корректной строки)
<img width="671" height="381" alt="lab5" src="https://github.com/user-attachments/assets/095b8430-2635-40da-8e63-ff005e177e5b" />

## Реализованные контекстно-зависимые условия
### 1. Уникальность идентификатора - каждый идентификатор должен быть объявлен 1 раз
```
DECLARE product_price CONSTANT INTEGER := 150;
DECLARE product_price CONSTANT INTEGER := 200;
```
<img width="1354" height="369" alt="image" src="https://github.com/user-attachments/assets/13474806-f2ea-4a01-94fa-d7837c5a3d8a" />


### 2. Использование идентификаторов - нельзя использовать идентификатор который не был объявлен
```
DECLARE product_price CONSTANT INTEGER := 150;
DECLARE product CONSTANT INTEGER := abc;
```
<img width="1338" height="340" alt="image" src="https://github.com/user-attachments/assets/4bf12a6b-672d-47b1-97b0-1f6d8e21855b" />


### 3. Использование идентификаторов - Проверить, что используемые идентификаторы были объявлены ранее (для выражений).
```
DECLARE final_price CONSTANT INTEGER := product_price;
```
<img width="898" height="340" alt="image" src="https://github.com/user-attachments/assets/de991719-ed26-490f-a421-d461edc4938d" />

### 4. Допустимые значения - значение должно лежать в допустимых пределах
```
DECLARE product_price CONSTANT INTEGER := 999999999999999999999;
```
<img width="1357" height="372" alt="image" src="https://github.com/user-attachments/assets/b8d0c5d6-42b9-4e77-b53e-669daa60e52d" />

## Структура AST
Типы узлов:
* AstNode - Базовый абстрактный класс для всех узлов дерева
* ConstDeclNode - Узел объявления константы
* IntNode - Узел типа данных. Используется для представления типа INTEGER
* IntLiteralNode - Узел целочисленного литерала. Используется, когда справа от оператора := находится числовое значение
* IdentifierNode - Узел ссылки на идентификатор. Используется, когда справа от оператора := находится ранее объявленный идентификатор
## Пример AST
Для строки:
```
DECLARE product_price CONSTANT INTEGER := 150;
DECLARE product CONSTANT INTEGER := -2500;
DECLARE tax CONSTANT INTEGER := product_price;
```

```
└── ConstDeclNode
    ├── name: "product_price"
    ├── modifiers: ["CONSTANT"]
    ├── type: IntNode
    │   └── name: "INTEGER"
    └── value: IntLiteralNode
        └── value: 150

└── ConstDeclNode
    ├── name: "product"
    ├── modifiers: ["CONSTANT"]
    ├── type: IntNode
    │   └── name: "INTEGER"
    └── value: IntLiteralNode
        └── value: -2500

└── ConstDeclNode
    ├── name: "tax"
    ├── modifiers: ["CONSTANT"]
    ├── type: IntNode
    │   └── name: "INTEGER"
    └── value: IdentifierNode
        └── name: "product_price"
```
<img width="334" height="529" alt="image" src="https://github.com/user-attachments/assets/90d60dcc-3113-44e4-bf8c-a3897394778a"/>

## Формат вывода
После нажатия кнопки пуск:
* выполняется анализ
* отображается AST
* выводятся ошибки с позициями





