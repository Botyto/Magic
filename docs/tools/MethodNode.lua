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

		if next(parameters or empty_table) then --2. parameters
			table.insert(page, writer:GenerateOrderedListLine("Parameters"))
			table.insert(page, self:GenerateParametersTable(writer, parameters))
		end

		if next(results or empty_table) then
			table.insert(page, writer:GenerateOrderedListLine("Results")) --3. results
			table.insert(page, self:GenerateParametersTable(writer, results))
		end

		writer:ResetOrderedList()
	end

	return page
end

function MethodNode:GenerateParametersTable(writer, list)
	local columns = { "Type", "Name", "Summary" }
	local t = { }
	for i,param in ipairs(list) do
		table.insert(t, { param.value_type, param.title, param.summary or "..." })
	end

	return writer:GenerateTable(t, columns)
end

----------------------------

Class.ParameterNode = {
	__inherit = "Node",

	value_type = false, --value type (node, string or `false`)
	default = false, --default value (string or `false`)
}

function ParameterNode:GenerateContent(writer)
	return ""
end
