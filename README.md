# Smart API that automatically creates databases per website, creates tables per form and inserts data to respective tables when the forms consume the APIs.
**To Test**
1. To submit new record, send Post request to server/CustomForm (e.g https://localhost:44394/CustomForm)
2. To retrieve form data per site, send get request to server/CustomForm, specifying the domain name and the form name (e.g   https://localhost:44394/CustomForm?formDomainName=local1&formName=Customers)