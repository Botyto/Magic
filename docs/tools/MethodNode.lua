require "CodeNode"

Class.MethodNode = {
	__inherit = "CodeNode",

	parameters = false, --node parameters (per variant)
	results = false, --returned values (per variant)
}

function MethodNode:GeneratePageContent(writer)
	local page = { }

	for i = 1, #self.variants do
		local variant = self.variants[i]
		local parameters = self.parameters[i]
		local results = self.results[i]
		table.insert(page, writer:GenerateSection("Description"))

		table.insert(page, writer:GenerateOrderedListLine("Syntax")) --1. syntax
		table.insert(page, writer:GenerateSnippet(variant, self.language, self.summary))

		table.insert(page, writer:GenerateOrderedListLine("Parameters")) --2. parameters
		table.insert(page, writer:GenerateParametersTable(writer))

		table.insert(page, writer:GenerateOrderedListLine("Results")) --3. results

		writer:ResetOrderedList()
	end

	return page
end

----------------------------

Class.ParameterNode = {
	__inherit = "Node",

	value_type = false, --value type (node, string or `false`)
}

function ParameterNode:GenerateContent(writer)
	return ""
end
