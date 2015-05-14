using System.Collections.Generic;

namespace PADIMapNoReduce {
    public interface IMapper {
        IList<KeyValuePair<string, string>> Map(string fileLine);
    }

}
