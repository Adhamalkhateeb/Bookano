using Bookano.Application.DTOs.Publishers;
using Bookano.Application.Validators.Common;
using FluentValidation;

namespace Bookano.Application.Validators;

public class PublisherFormDtoValidator : CommonValidator<PublisherFormDto>
{
    public PublisherFormDtoValidator() : base(x => x.Name) { }
}