# Лабораторная работа №6 — Создание внутренней формы представления программы
## Цель работы
Изучить методы построения внутреннего представления программы (ВПП) на основе контекстно-свободной грамматики, реализовать синтаксический анализатор методом рекурсивного спуска и преобразовать арифметические выражения в тетрады и ПОЛИЗ.
## Автор
* Марченко А.Е.
* Группа: АП-326
## Вариант задания
Язык: C# (PostgreSQL (SQL))
Определение грамматики:
```
E → TA
A → ε | + TA | - TA
T → FB
B → ε | * FB | / FB
F → num | id | (E)
id → letter {letter | digit | _}
num → digit {digit}

```
Примеры верных строк:
* 2+3
* (2+3)*4
* 8/(4-2)
* 15-3*2+8/4
* 2+3*4
## Диаграмма лексера
<img width="711" height="1101" alt="lab6" src="https://github.com/user-attachments/assets/df6abe1a-e1c7-4e00-b5c4-fac87e3dd5ee" />

## Тестовые примеры работы лексера и парсера
### Строка: 15-3*2+8/4 (корректная строка)
### <img width="642" height="183" alt="image" src="https://github.com/user-attachments/assets/0d04f009-7c28-4b2a-afd4-8063fa8ce9b3" />

#### <img width="552" height="105" alt="image" src="https://github.com/user-attachments/assets/0191940c-f89b-43bd-a058-8ce29e3dbbee" />

### Строка: 2 @ 3 (лексическая ошибка)
### <img width="727" height="544" alt="image" src="https://github.com/user-attachments/assets/a58a17a2-b227-483a-932d-f24d908a48fa" />

### Строка: 2+ + (синтаксическая ошибка)
<img width="846" height="558" alt="image" src="https://github.com/user-attachments/assets/0638d575-fce2-4eaa-a4f5-7bb3676fa4a1" />

## Внутренняя форма представления программы (тетрады)
### Строка: 15-3*2+8/4
<img width="632" height="170" alt="image" src="https://github.com/user-attachments/assets/039fef7b-33cb-4a88-9b48-c286b3e77d91" />

### Строка: (2+3)*4
<img width="644" height="124" alt="image" src="https://github.com/user-attachments/assets/a01b4eba-8d1e-4a0d-bcd1-c94de05eb708" />


## ПОЛИЗ
### Строка: 15-3*2+8/4
<img width="549" height="86" alt="image" src="https://github.com/user-attachments/assets/8bbb78f3-6cd8-4c25-8fab-810dbf931744" />


