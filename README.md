# Drupal8Services-CSharp-Example

This class was created in order to communicate with Drupal 8 REST
services. It was developed against version 8.1.1

This class uses BASIC AUTH. If you are using this on the Internet
you should make sure all communication is https.
Basic auth includes base64 encoded username and password with each
request.

This class also focuses on HAL JSON.

In Drupal 8, you can use Basic Auth with no CSRF token when using GET

When using POST we have to also utilize CSRF tokens

Should be able to be used in C# applications; but targeted for use in Unity3D.

THIS CLASS BLOCKS FOR AN UNDEFINED AMOUNT OF TIME WHILE PREFORMING THE WEB REQUESTS

## Example
`
DrupalInterface _drup = new DrupalInterface("http://127.0.0.1","YourUsername","YourPassword");
// Get the data in node 1 using basic auth from the drupal site at 127.0.0.1
_drup.getNode(1);
`