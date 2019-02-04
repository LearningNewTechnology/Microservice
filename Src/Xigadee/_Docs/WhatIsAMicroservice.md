<table>
<tr>
<td width="80%"><a href="../../../README.md"><img src="../../../docs/X2a.png" alt="Xigadee"></a></td>
<td width = "*" align="right"><img src="../../../docs/smallWIP.jpg" alt="Sorry, I'm still working here" height="100"></td>
</tr>
</table>

# What is a Microservice?

Microservices are a relatively recent concept that change the way in how we design and build software systems. 
They are essentially small autonomous services that work together to form an overall solution.

Instead of designing your application as one big tightly coupled system, we break it down in to smaller composable piece, 
that can be designed, tested, and deployed independently from each other. 

Microservices have grown out of the experience of building large SOA-based systems, and seek to remedy much of the problems caused by that approach.
However, using a Microservice architecture brings challenges of its own which the Xigadee Framework was built to address.

In general Microservices have the following properties:

1. **Complete and Minimal**
	- A Microservice should deliver a small - but complete - set of business capability built ideally around a specific business domain. 
	An example of this would be an eCommerce solution, where we create a Customer Microservice that encapsulated the management of a customer's details, 
    with another Microservice created to handle the customer's purchases. These services could be updated independently without affecting the other parts of the system.
	Basically, a Microservice should do one job well, not a multitude of disparate unrelated tasks. In a sense this is similar to the [Single Responsibility Principle](https://en.wikipedia.org/wiki/Single_responsibility_principle) for software development, but on a slightly larger scale.
	A Microservice should have the ability to be independently updated without affecting the rest of the application.	
2. **Scalable and Elastic**
	- Generally when the load on a particular Microservice increases, 
    the technology that implements the service should allow it to scale-out to multiple instances to handle this additional load and then scale back when the load reduces. 
    This is especially important for systems that have different operating characteristics over time, 
    i.e. night time batch loads are different to daytime loads. 
    This way we don't have to plan for the maximum possible throughput, but can be more flexible and adjust our capability when required.
3. **Resilient**
	- Microservices based systems should designed to cope with failure, specifically where a specific Microservice is temporarily unavailable. 
    The overall system and should be eventually consistent. 
4. **Composable**
	- One of the [key](https://en.wikipedia.org/wiki/Composability) benefits of using Microservices, is that it allows for the reuse of the Microservice in other applications or services. 
    We are building a capability that can be consumed when needed, i.e. a Customer Microservice. How we consolidate that service in to the application can be changed and adjusted over time. We now have a Customer capability, but we are open to integrate that in to other applications as our needs change, without the worry of breaking existing functionality as this service is not tightly coupled in to a specific business function.

## How is that different from before?

The [Monolith](https://en.wikipedia.org/wiki/Monolithic_application).

## The Gotcha law!

It's important to understand [CAP Theorem](https://en.wikipedia.org/wiki/CAP_theorem) when building a Microservice based application. If you don't them things can get very complicated and messy, very fast. Building a Microservice is relatively easy; building a Microservice to recover gracefully when things go wrong, is significantly more complicated. 

## The Xigadee approach to Microservices

Xigadee has been built from our experience. Many of the problems that we have faced building commercial enterprise-grade Microservice based solutions have been incorporated in to the framework. With these types of application, specifically when using PASS based technologies, fault tolerance is key. Xigadee solves many of those challenges for you, and provides an extensible framework to allow you to extend existing systems and services when required.

<img src="Images/Microservice.png" alt="Message Flow" height="500"/>

## Next: [Introduction To Xigadee](Introduction.md)

##### Footnote, thanks & further reading

 - [Mr Fowler](https://martinfowler.com/articles/microservices.html)
 - [Nirmata](http://www.nirmata.com/2015/02/microservices-five-architectural-constraints/)

<table><tr> 
<td><a href="http://www.hitachiconsulting.com"><img src="../../../docs/hitachi.png" alt="Hitachi Consulting" height="50"/></a></td> 
<td>Created by: <a href="http://github.com/paulstancer">Paul Stancer</a></td>
  <td><a href="../../../README.md">Home</a></td>
</tr></table>
